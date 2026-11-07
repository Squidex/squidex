/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable, signal, untracked } from '@angular/core';
import { Types } from '../utils/types';

export type ButtonItem = { owner: any; name: string; method: () => void } & ButtonOptions;
export type ButtonOptions = { disabled?: boolean; color?: string };

@Injectable()
export class ToolbarService {
    private readonly buttonsValue = signal<ReadonlyArray<ButtonItem>>([]);

    public readonly buttons = this.buttonsValue.asReadonly();

    public addButton(owner: any, name: string, method: () => void, options?: ButtonOptions) {
        const newButton = { owner, name, method, disabled: options?.disabled, color: options?.color || 'primary' };

        const buttons = untracked(this.buttonsValue);
        const button = buttons.find(x => x.name === name);

        if (!button || !Types.equals(newButton, button)) {
            const newButtons = buttons.filter(x => x.name !== name);

            newButtons.push(newButton);

            this.buttonsValue.set(newButtons);
        }
    }

    public remove(owner: any) {
        const buttons = untracked(this.buttonsValue);

        const newButtons = buttons.filter(x => x.owner !== owner);

        if (newButtons.length !== buttons.length) {
            this.buttonsValue.set(newButtons);
        }
    }

    public removeAll() {
        const buttons = untracked(this.buttonsValue);

        if (buttons.length > 0) {
            this.buttonsValue.set([]);
        }
    }
}
