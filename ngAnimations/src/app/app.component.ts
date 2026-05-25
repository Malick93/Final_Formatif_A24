import { transition, trigger, useAnimation } from '@angular/animations';
import { Component } from '@angular/core';
import { bounce, shake, shakeX, tada } from 'ng-animate';
import { lastValueFrom, timer } from 'rxjs';

@Component({
    selector: 'app-root',
    templateUrl: './app.component.html',
    styleUrls: ['./app.component.css'],
    animations:[
    trigger('bounce', [
      transition(':increment', useAnimation(bounce, {params: {timing: 4}}))
    ]),
    trigger('shake', [
      transition(':increment', useAnimation(shake, {params: {timing: 2}}))]
    ),
    trigger('tada', [
      transition(':increment', useAnimation(tada, {params: {timing: 3}}))
    ])
  ],
    standalone: true
})
export class AppComponent {
  title = 'ngAnimations';

  ng_bounce = 0;
  ng_shake = 0;
  ng_tada = 0;
  rotate = false;
  mavariable = 0;
  shake = false;

  constructor() {
  }

  async waitFor(delayInSeconds: number) {
    await lastValueFrom(timer(delayInSeconds * 1000));
  }

  async toutAnimer() {
    this.ng_shake++;
    await this.waitFor(2);
    this.ng_bounce++;
    await this.waitFor(3);
    this.ng_tada++;
  }
  async boucleAnimer() {
    this.toutAnimer()
    setTimeout(() => {
      this.boucleAnimer();
    }, 8000)
  }

  rotateMe(){
    this.rotate = true;
    setTimeout(() => {this.rotate = false;}, 1000)
  }
}
