import { Routes } from '@angular/router';

export const routes: Routes = [
    { path: 'send', loadComponent: () => import('./send/send.component').then(c => c.SendComponent) },
    { path: '**', redirectTo: 'send' }
];
