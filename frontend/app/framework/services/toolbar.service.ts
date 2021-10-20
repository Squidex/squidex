/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { Types } from './../utils/types';

export type ButtonItem = { owner: any; name: string; method: () => void } & ButtonOptions;
export type ButtonOptions = { disabled?: boolean; color?: string };

@Injectable()
export class ToolbarService {
    private readonly buttons$ = new BehaviorSubject<ReadonlyArray<ButtonItem>>([]);

    public get buttonsChanges(): Observable<ReadonlyArray<ButtonItem>> {
        return this.buttons$;
    }

    public addButton(owner: any, name: string, method: () => void, options?: ButtonOptions) {
        const newButton = { owner, name, method, disabled: options?.disabled, color: options?.color || 'primary' };

        const buttons = this.buttons$.value;
        const button = buttons.find(x => x.name === name);

        if (!button || !Types.equals(newButton, button)) {
            const newButtons = this.buttons$.value.filter(x => x.name !== name);

            newButtons.push(newButton);

            this.buttons$.next(newButtons);
        }
    }

    public remove(owner: any) {
        const buttons = this.buttons$.value;

        const newButtons = buttons.filter(x => x.owner !== owner);

        if (newButtons.length !== buttons.length) {
            this.buttons$.next(newButtons);
        }
    }

    public removeAll() {
        const buttons = this.buttons$.value;

        if (buttons.length > 0) {
            this.buttons$.next([]);
        }
    }
}
