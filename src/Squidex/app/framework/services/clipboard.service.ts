/*
 *PinkParrot CMS
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
    private textInstance = new BehaviorSubject<string>('');

    public get text(): Observable<string> {
        return this.textInstance;
    }

    public selectText(): string {
        let result = '';

        this.textInstance.subscribe(t => {
            result = t;
        }).unsubscribe();

        return result || '';
    }

    public setText(text: any) {
        this.textInstance.next(text);
    }
}