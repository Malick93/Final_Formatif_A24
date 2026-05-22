using BackgroundServiceMath.Data;
using BackgroundServiceMath.Models;
using BackgroundServiceVote.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace BackgroundServiceMath.Services;

public class UserData
{
    public int Choice { get; set; } = -1;
    public int NbConnections { get; set; } = 0;
}

public class MathBackgroundService : BackgroundService
{
    public const int DELAY = 20 * 1000;

    private Dictionary<string, UserData> _data = new();

    private IHubContext<MathQuestionsHub> _mathQuestionHub;

    private MathQuestion? _currentQuestion;

    public MathQuestion? CurrentQuestion => _currentQuestion;

    private MathQuestionsService _mathQuestionsService;

    private IServiceScopeFactory _serviceScopeFactory;

    public MathBackgroundService(IHubContext<MathQuestionsHub> mathQuestionHub, MathQuestionsService mathQuestionsService, IServiceScopeFactory serviceScopeFactory)
    {
        _mathQuestionHub = mathQuestionHub;
        _mathQuestionsService = mathQuestionsService;
        _serviceScopeFactory = serviceScopeFactory;
    }

    public void AddUser(string userId)
    {
        if (!_data.ContainsKey(userId))
        {
            _data[userId] = new UserData();
        }
        _data[userId].NbConnections++;
    }

    public void RemoveUser(string userId)
    {
        if (!_data.ContainsKey(userId))
        {
            _data[userId].NbConnections--;
            if (_data[userId].NbConnections <= 0)
                _data.Remove(userId);
        }
    }

    public async void SelectChoice(string userId, int choice)
    {
        if (_currentQuestion == null)
            return;

        UserData userData = _data[userId];

        if (userData.Choice != -1)
            throw new Exception("A user cannot change is choice!");

        userData.Choice = choice;

        _currentQuestion.PlayerChoices[choice]++;

        // TODO: Notifier les clients qu'un joueur a choisi une réponse
        _mathQuestionHub.Clients.All.SendAsync("IncreasePlayersChoices", userData.Choice);
    }

    private async Task EvaluateChoices()
    {
        // On crée un scope pour pouvoir utiliser le DbContext (service scoped) depuis ce singleton
        using IServiceScope scope = _serviceScopeFactory.CreateScope();
        BackgroundServiceContext db = scope.ServiceProvider.GetRequiredService<BackgroundServiceContext>();

        foreach (var userId in _data.Keys)
        {
            var userData = _data[userId];

            if (userData.Choice == _currentQuestion!.RightAnswerIndex)
            {
                // Incrémenter NbRightAnswers dans la BD
                Player player = await db.Player.SingleAsync(p => p.UserId == userId);
                player.NbRightAnswers++;

                // Notifier le client qu'il a eu la bonne réponse
                await _mathQuestionHub.Clients.User(userId).SendAsync("CorrectAnswer");
            }
            else
            {
                // Notifier le client qu'il a eu la mauvaise réponse
                await _mathQuestionHub.Clients.User(userId).SendAsync("WrongAnswer");
            }
        }

        // Sauvegarder tous les changements en une seule fois
        await db.SaveChangesAsync();

        // Reset
        foreach (var key in _data.Keys)
        {
            _data[key].Choice = -1;
        }
    }

    private async Task Update(CancellationToken stoppingToken)
    {
        if (_currentQuestion != null)
        {
            await EvaluateChoices();
        }

        _currentQuestion = _mathQuestionsService.CreateQuestion();

        await _mathQuestionHub.Clients.All.SendAsync("CurrentQuestion", _currentQuestion);
    }


    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Update(stoppingToken);
            await Task.Delay(DELAY, stoppingToken);
        }
    }
}