import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface CreateUserPayload {
    username: string;
    email: string;
    password: string;
    confirmPassword: string;
    fullName: string;
    phoneNumber: string;
}

@Injectable({
    providedIn: 'root',
})
export class UserService {
    private http = inject(HttpClient);
    private apiUrl = 'http://localhost:5203/api/Auth';

    createUser(userData: CreateUserPayload): Observable<any> {
        return this.http.post<any>(`${this.apiUrl}/register`, userData);
    }
}
