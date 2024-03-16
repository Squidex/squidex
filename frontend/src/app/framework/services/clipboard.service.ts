/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';

@Injectable()
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
