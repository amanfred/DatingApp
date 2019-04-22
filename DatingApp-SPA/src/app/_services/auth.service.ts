import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import {map} from 'rxjs/operators';
import {JwtHelperService} from '@auth0/angular-jwt';

@Injectable({
  // this allows us to inject things into our service. Components are automatically injectable (by default).
  providedIn: 'root' // this tells our service which module is providing the serrvice. Wee need to add reference into app.module.ts
})
export class AuthService {
  baseUrl = 'http://localhost:5000/api/auth/'; // use base url in post method
  jwtHelper = new JwtHelperService();
  decodedToken: any;

  constructor(private http: HttpClient) { }

  login(model: any) {
    // provide the url. Application/json is selected by default. Token will be returned as response. We need to do something with it
    return this.http.post(this.baseUrl + 'login', model)
    .pipe( // this allows us to chain rxjs operators
      map((response: any) => {
        const user = response; // this is the token object
        if (user) {
          localStorage.setItem('token', user.token);
          this.decodedToken = this.jwtHelper.decodeToken(user.token);
        }
      })
    );
  }

  register(model: any) {
    return this.http.post(this.baseUrl + 'register', model);
  }

  loggedIn() {
    const token = localStorage.getItem('token');
    return !this.jwtHelper.isTokenExpired(token); // if it is expired it will return false.
  }
}
