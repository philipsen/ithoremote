import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { HttpClient } from '@angular/common/http';
import { ConfigLoaderService } from './config-loader.service';

@Injectable({
  providedIn: 'root'
})
export class RemoteCommandService {

  private url = this.configLoaderService.apiUrl;

  sendCommandBytes(house: string, remoteId: string, remoteCommand: string): Observable<Object> {
    const url = this.url + '/api/house/command/' + house + '/' + remoteId;
    return this.http.put(url, remoteCommand);
  }

  constructor(private http: HttpClient, private configLoaderService: ConfigLoaderService) { }
}
