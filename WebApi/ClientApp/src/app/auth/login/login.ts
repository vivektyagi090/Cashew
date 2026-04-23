import { Component, inject, effect, OnInit, OnDestroy, Renderer2 } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthStore } from '../../core/state/auth.store';
@Component({
  selector: 'app-login',
  imports: [CommonModule, FormsModule],
  templateUrl: './login.html',
  styleUrl: './login.css',
})
export class Login implements OnInit, OnDestroy {
  private authStore = inject(AuthStore) as InstanceType<typeof AuthStore>;
  private router = inject(Router);

  email = '';
  password = '';
  mobileNumber = '';
  loginMode: 'email' | 'mobile' = 'email';

  loading = this.authStore.loading;
  error = this.authStore.error;

  constructor() {
    effect(() => {
      if (this.authStore.isAuthenticated()) {
        this.router.navigateByUrl('/dashboard');
      }
    });
  }

  ngOnInit() {
    document.body.classList.add('vvk-portal-active');
  }

  ngOnDestroy() {
    document.body.classList.remove('vvk-portal-active');
  }

  setLoginMode(mode: 'email' | 'mobile') {
    this.loginMode = mode;
  }

  login() {
    this.authStore.login({
      usernameOrEmail: this.loginMode === 'email' ? this.email : this.mobileNumber,
      password: this.password,
      rememberMe: false
    });
  }

  googleLogin() {
    console.log('Redirecting to Google Auth...');
    // Future implementation: this.authStore.googleLogin();
  }
}
