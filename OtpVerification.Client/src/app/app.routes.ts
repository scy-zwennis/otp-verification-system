import { Routes } from '@angular/router';

export const routes: Routes = [
    { path: 'send', loadComponent: () => import('./send/send.component').then(c => c.SendComponent) },
    { path: 'validate', loadComponent: () => import('./validate/validate.component').then(c => c.ValidateComponent) },
    { path: '**', redirectTo: 'send' }
];
