import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';

import { Observable, of } from 'rxjs';
import { catchError, tap } from 'rxjs/operators';

// import { MessageService } from './message.service';

@Injectable({
  providedIn: 'root'
})
export class RemoteCommandService {

  private url = '/api/command/';

  constructor(
    private http: HttpClient,
    // private messageService: MessageService
  ) { }

  sendCommand(house: string, room: string, cmd: string): Observable<any> {
    const url = this.url + house + '/' + room + '/' + cmd;
    return this.http.get(url).pipe(
      tap(_ => this.log(`send command ${url}`)),
      catchError(this.handleError<any>('send command2')
    ));
  }

  sendCommandBytes(house: string, remoteId: string, remoteCommand: string): Observable<any> {
    const url = this.url + 'sendBytes/' + house + '/' + remoteId + '/' + remoteCommand;
    return this.http.get(url).pipe(
      tap(_ => this.log(`send command ${url}`)),
      catchError(this.handleError<any>('send command3')
    ));
  }

  private handleError<T> (operation = 'operation', result?: T) {
    return (error: any): Observable<T> => {

      // TODO: send the error to remote logging infrastructure
      console.error(error); // log to console instead

      // TODO: better job of transforming error for user consumption
      this.log(`${operation} failed: ${error.message}`);

      // Let the app keep running by returning an empty result.
      return of(result as T);
    };
  }

    /** Log a HeroService message with the MessageService */
    private log(message: string) {
      // this.messageService.add('HouseService: ' + message);
      console.log('log: ' + message);
    }
}
