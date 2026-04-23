import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private http = inject(HttpClient);

  private apiUrl = 'http://localhost:5203/api/Auth';

  // login(payload: { email: string; password: string }): Observable<any> {
  //   debugger;
  //   return this.http.post<any>(this.apiUrl, payload);
  // }
  login(payload: { usernameOrEmail: string; password: string; rememberMe?: boolean }): Observable<any> {
    // debugger; // Helpful for verifying the payload right before it leaves
    return this.http.post<any>(`${this.apiUrl}/login`, payload);
  }

  register(payload: any): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/register`, payload);
  }
}
