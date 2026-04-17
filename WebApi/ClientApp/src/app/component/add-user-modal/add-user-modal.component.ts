import { Component, Output, EventEmitter, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { UserService, CreateUserPayload } from '../../core/services/user.service';

@Component({
    selector: 'app-add-user-modal',
    imports: [CommonModule, FormsModule],
    templateUrl: './add-user-modal.component.html',
    styleUrl: './add-user-modal.component.css',
})
export class AddUserModalComponent {
    @Output() close = new EventEmitter<void>();

    private userService = inject(UserService);

    // Form data
    formData = {
        username: '',
        email: '',
        password: '',
        confirmPassword: '',
        fullName: '',
        phoneNumber: ''
    };

    // State
    loading = signal(false);
    error = signal<string | null>(null);
    success = signal(false);

    // Validation errors
    validationErrors = {
        username: '',
        email: '',
        password: '',
        confirmPassword: '',
        fullName: '',
        phoneNumber: ''
    };

    onClose() {
        this.close.emit();
    }

    validateForm(): boolean {
        let isValid = true;

        // Reset errors
        Object.keys(this.validationErrors).forEach(key => {
            this.validationErrors[key as keyof typeof this.validationErrors] = '';
        });

        // Username validation
        if (!this.formData.username.trim()) {
            this.validationErrors.username = 'Username is required';
            isValid = false;
        }

        // Email validation
        if (!this.formData.email.trim()) {
            this.validationErrors.email = 'Email is required';
            isValid = false;
        } else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(this.formData.email)) {
            this.validationErrors.email = 'Invalid email format';
            isValid = false;
        }

        // Password validation
        if (!this.formData.password) {
            this.validationErrors.password = 'Password is required';
            isValid = false;
        } else if (this.formData.password.length < 6) {
            this.validationErrors.password = 'Password must be at least 6 characters';
            isValid = false;
        }

        // Confirm password validation
        if (!this.formData.confirmPassword) {
            this.validationErrors.confirmPassword = 'Please confirm password';
            isValid = false;
        } else if (this.formData.password !== this.formData.confirmPassword) {
            this.validationErrors.confirmPassword = 'Passwords do not match';
            isValid = false;
        }

        // Full name validation
        if (!this.formData.fullName.trim()) {
            this.validationErrors.fullName = 'Full name is required';
            isValid = false;
        }

        // Phone number validation
        if (!this.formData.phoneNumber.trim()) {
            this.validationErrors.phoneNumber = 'Phone number is required';
            isValid = false;
        } else if (!/^\d{10}$/.test(this.formData.phoneNumber)) {
            this.validationErrors.phoneNumber = 'Phone number must be 10 digits';
            isValid = false;
        }

        return isValid;
    }

    onSubmit() {
        if (!this.validateForm()) {
            return;
        }

        this.loading.set(true);
        this.error.set(null);

        const payload: CreateUserPayload = {
            username: this.formData.username,
            email: this.formData.email,
            password: this.formData.password,
            confirmPassword: this.formData.confirmPassword,
            fullName: this.formData.fullName,
            phoneNumber: this.formData.phoneNumber
        };

        this.userService.createUser(payload).subscribe({
            next: (response) => {
                this.loading.set(false);
                this.success.set(true);

                // Close modal after 2 seconds
                setTimeout(() => {
                    this.onClose();
                }, 2000);
            },
            error: (err) => {
                this.loading.set(false);
                this.error.set(err.error?.message || 'Failed to create user. Please try again.');
            }
        });
    }
}
