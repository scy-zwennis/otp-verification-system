import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { delay, from, Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class OneTimePinService {
  private http = inject(HttpClient);

  public send = (email: string): Observable<unknown> =>
    this.http.post('https://scy-8081.entrostat.dev/OneTimePin/Send', { email });
}
