import {
  Component,
  input,
} from '@angular/core';

@Component({
  selector: 'app-feedback',

  template: `
    @if (message()) {
      <p
        role="status"
        [class.error]="error()"
        [class.success]="!error()">

        {{ message() }}

      </p>
    }
  `,
})
export class FeedbackComponent {

  readonly message = input('');

  readonly error = input(true);
}