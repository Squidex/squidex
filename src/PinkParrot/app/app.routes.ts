import * as Ng2 from '@angular/core';
import * as Ng2Router from '@angular/router';

import {
    AppsComponent,
    LoginComponent
} from './components';

import { 
    AuthGuard
} from './shared';

export const routes: Ng2Router.Routes = [
    {
        path: '',
        redirectTo: 'apps',
        pathMatch: 'full'
    },
    {
        path: 'apps',
        component: AppsComponent,
        canActivate: [AuthGuard]
    },
    {
        path: 'login',
        component: LoginComponent
    }
];

export const routing: Ng2.ModuleWithProviders = Ng2Router.RouterModule.forRoot(routes, { useHash: true });