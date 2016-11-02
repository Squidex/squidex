import { TestBed } from '@angular/core/testing';

import { RouterModule, provideRoutes } from '@angular/router';
import { RouterTestingModule } from '@angular/router/testing';

import { SqxLayoutModule } from './components/layout';

import { AppsStoreService } from './shared';

import { AppComponent } from './app.component';

describe('App', () => {
    beforeEach(() => {
        TestBed.configureTestingModule({ 
            declarations: [
                AppComponent
            ],
            imports: [
                RouterModule,
                RouterTestingModule,
                SqxLayoutModule
            ],
            providers: [
                provideRoutes([]),
                { provide: AppsStoreService, useValue: new AppsStoreService(null, null) }
            ]
        });
    });
    
    it('should work', () => {
        const fixture = TestBed.createComponent(AppComponent);

        expect(fixture.componentInstance instanceof AppComponent).toBe(true, 'should create AppComponent');
    });
});