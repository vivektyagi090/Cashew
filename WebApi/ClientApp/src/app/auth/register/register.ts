import { Component, inject, effect, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthStore } from '../../core/state/auth.store';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './register.html',
  styleUrl: '../login/login.css', // Reusing login styles
})
export class Register implements OnInit, OnDestroy {
  public authStore = inject(AuthStore) as InstanceType<typeof AuthStore>;
  private router = inject(Router);

  fullName = '';
  email = '';
  username = '';
  password = '';
  confirmPassword = '';

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

  register() {
    if (this.password !== this.confirmPassword) {
      // Manual error handling for password mismatch
      return;
    }

    this.authStore.register({
      fullName: this.fullName,
      email: this.email,
      username: this.username,
      password: this.password,
      confirmPassword: this.confirmPassword
    });
  }
}
