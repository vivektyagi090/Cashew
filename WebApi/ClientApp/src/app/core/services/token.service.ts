import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
@Injectable({
  providedIn: 'root',
})
export class Token {
  private apiUrl = 'http://localhost:5203/api/Auth/login';

  constructor(private http: HttpClient) { }

  login(data: any): Observable<any> {
    return this.http.post(this.apiUrl, data);
  }
}
