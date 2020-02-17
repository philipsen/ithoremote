import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { HttpClient } from '@angular/common/http';

@Injectable({
  providedIn: 'root'
})
export class RemoteCommandService {

  private url = 'http://localhost:5000/api/';

  sendCommandBytes(house: string, remoteId: string, remoteCommand: string): Observable<Object> {
    console.log('remote command send: ' + house + ' -> ' + remoteId + '::' + remoteCommand);
    const url = this.url + 'house/command/' + house + '/' + remoteId + '/' + remoteCommand;
    return this.http.put(url, remoteCommand);
  }

  constructor(private http: HttpClient) { }
}
