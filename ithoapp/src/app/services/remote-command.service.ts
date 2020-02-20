import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { HttpClient } from '@angular/common/http';

@Injectable({
  providedIn: 'root'
})
export class RemoteCommandService {

  private url = 'https://wmt6-pi4:5001/api/';

  sendCommandBytes(house: string, remoteId: string, remoteCommand: string): Observable<Object> {
    console.log('remote command send: ' + house + ' -> ' + remoteId + '::' + remoteCommand);
    const url = this.url + 'house/command/' + house + '/' + remoteId;
    return this.http.put(url, remoteCommand);
  }

  constructor(private http: HttpClient) { }
}
