import {
  Component,
  input,
} from '@angular/core';

@Component({
  selector: 'app-feedback',

  templateUrl: './feedback.component.html',
  styleUrl: './feedback.component.css',
})
export class FeedbackComponent {

  readonly message = input('');

  readonly error = input(true);
}