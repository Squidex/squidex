/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import * as Ng2 from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';

export const ClipboardServiceFactory = () => {
    return new ClipboardService();
};

@Ng2.Injectable()
export class ClipboardService {
    private readonly text$ = new BehaviorSubject<string>('');

    public get textChanges(): Observable<string> {
        return this.text$;
    }

    public selectText(): string {
        let result = '';

        this.text$.subscribe(t => {
            result = t;
        }).unsubscribe();

        return result || '';
    }

    public setText(text: any) {
        this.text$.next(text);
    }
}