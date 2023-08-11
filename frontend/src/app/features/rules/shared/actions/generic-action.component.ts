/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { EMPTY, Observable, shareReplay } from 'rxjs';
import { ActionForm, RulesService, ScriptCompletions, TypedSimpleChanges } from '@app/shared';

@Component({
    selector: 'sqx-generic-action',
    styleUrls: ['./generic-action.component.scss'],
    templateUrl: './generic-action.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class GenericActionComponent {
    @Input({ required: true })
    public actionForm!: ActionForm;

    @Input({ required: true })
    public appName!: string;

    @Input({ required: true })
    public trigger!: any;

    @Input({ required: true })
    public triggerType: string | undefined | null;

    public ruleCompletions: Observable<ScriptCompletions> = EMPTY;

    constructor(
        private readonly rulesService: RulesService,
    ) {
    }

    public ngOnChanges(changes: TypedSimpleChanges<this>) {
        if (changes.appName || changes.triggerType) {
            if (this.triggerType) {
                this.ruleCompletions = this.rulesService.getCompletions(this.appName, this.triggerType).pipe(shareReplay(1));
            } else {
                this.ruleCompletions = EMPTY;
            }
        }
    }
}
