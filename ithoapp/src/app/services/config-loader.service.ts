import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { map, tap } from 'rxjs/operators';
import { AppConfig } from '../models/config.model';

@Injectable()
export class ConfigLoaderService {

public apiUrl = 'Not Set Yet';

  constructor(private httpClient: HttpClient) { }

  initialize() {
    return this.httpClient.get<AppConfig>('./assets/config.json')
    .pipe(tap((response: AppConfig) => {
      this.apiUrl = response.apiurl;
    })).toPromise<AppConfig>();
  }
}
