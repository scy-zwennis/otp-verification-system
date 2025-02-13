import { CommonModule } from '@angular/common';
import { Component, DestroyRef, inject, OnInit } from '@angular/core';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { OneTimePinService } from '../services/one-time-pin.service';
import { catchError, finalize } from 'rxjs';
import { ErrorHelper } from '../helpers/error.helper';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { LoadingButtonDirective } from '../directives/loading-button.directive';

@Component({
  selector: 'app-validate',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule, LoadingButtonDirective],
  templateUrl: './validate.component.html',
  styleUrl: './validate.component.scss'
})
export class ValidateComponent implements OnInit {
  private oneTimePinService = inject(OneTimePinService);
  private route = inject(ActivatedRoute);
  private destoryRef = inject(DestroyRef);

  form: FormGroup;
  formSubmitted = false;

  isLoading = false;
  successMessage = "";
  errorMessage = "";

  constructor() {
    this.form = new FormGroup({
      email: new FormControl('', [Validators.required, Validators.email]),
      code: new FormControl('', [Validators.required, Validators.minLength(6), Validators.maxLength(6)])
    });
  }

  ngOnInit(): void {
    this.route.queryParams.pipe(takeUntilDestroyed(this.destoryRef)).subscribe(params => {
      const email = params['email'] || '';
      this.form.get('email')?.setValue(email);
    });
  }

  requestOtp(): void {
    this.formSubmitted = true;
    if (!this.form.valid || this.isLoading) {
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';
    this.successMessage = '';

    this.oneTimePinService.validate(this.form.value)
      .pipe(
        catchError(error => {
          this.errorMessage = ErrorHelper.formatHttpError(error);
          throw error;
        }),
        finalize(() => this.isLoading = false),
        takeUntilDestroyed(this.destoryRef))
      .subscribe(() => {
        this.successMessage = "OTP is valid";
      });
  }

  hasError(controlName: string): boolean {
    const control = this.form.get(controlName);
    if (!control) {
      return false;
    }

    return this.formSubmitted ? control.invalid : control.invalid && control.touched;
  }
}
