import { CommonModule } from '@angular/common';
import { Component, DestroyRef, inject, OnDestroy } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop'
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { OneTimePinService } from '../services/one-time-pin.service';
import { catchError, finalize, Subject, takeUntil, timer } from 'rxjs';
import { ErrorHelper } from '../helpers/error.helper';
import { RouterModule } from '@angular/router';
import { LoadingButtonDirective } from '../directives/loading-button.directive';

@Component({
  selector: 'app-send',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule, LoadingButtonDirective],
  templateUrl: './send.component.html',
  styleUrl: './send.component.scss'
})
export class SendComponent implements OnDestroy {
  private oneTimePinService = inject(OneTimePinService);

  private countdownTimerStop$ = new Subject<void>();

  form: FormGroup;
  formSubmitted = false;

  isLoading = false;
  hasSentOtp = false;
  countdown = 0;
  errorMessage = "";

  get email(): string {
    return this.form.value.email;
  }

  constructor(private destoryRef: DestroyRef) {
    this.form = new FormGroup({
      email: new FormControl('', [Validators.required, Validators.email])
    });
  }

  ngOnDestroy(): void {
    this.countdownTimerStop$.next();
    this.countdownTimerStop$.complete();
  }

  requestOtp(): void {
    this.formSubmitted = true;
    if (!this.form.valid || this.isLoading) {
      return;
    }

    const { email } = this.form.value;

    this.isLoading = true;
    this.errorMessage = '';

    this.oneTimePinService.send(email)
      .pipe(
        catchError(error => {
          this.errorMessage = ErrorHelper.formatHttpError(error);
          throw error;
        }),
        finalize(() => this.isLoading = false),
        takeUntilDestroyed(this.destoryRef))
      .subscribe(() => {
        this.hasSentOtp = true;
        this.startCountdown();
      });
  }

  hasError(controlName: string): boolean {
    const control = this.form.get(controlName);
    if (!control) {
      return false;
    }

    return this.formSubmitted ? control.invalid : control.invalid && control.touched;
  }

  private startCountdown(seconds: number = 5) {
    this.countdownTimerStop$.next();
    this.countdown = seconds;

    timer(0, 1000).pipe(takeUntil(this.countdownTimerStop$))
      .subscribe(() => this.countdown--);
  }
}
