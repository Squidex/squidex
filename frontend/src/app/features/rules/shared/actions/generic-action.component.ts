/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, Input, OnChanges, SimpleChanges } from '@angular/core';
import { EMPTY, Observable, shareReplay } from 'rxjs';
import { ActionForm, RuleCompletions, RulesService } from '@app/shared';

@Component({
    selector: 'sqx-generic-action[actionForm][appName][trigger][triggerType]',
    styleUrls: ['./generic-action.component.scss'],
    templateUrl: './generic-action.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class GenericActionComponent implements OnChanges {
    @Input()
    public actionForm!: ActionForm;

    @Input()
    public appName!: string;

    @Input()
    public trigger!: any;

    @Input()
    public triggerType: string | undefined | null;

    public ruleCompletions: Observable<RuleCompletions> = EMPTY;

    constructor(
        private readonly rulesService: RulesService,
    ) {
    }

    public ngOnChanges(changes: SimpleChanges) {
        if (changes['appName'] || changes['triggerType']) {
            if (this.triggerType) {
                this.ruleCompletions = this.rulesService.getCompletions(this.appName, this.triggerType).pipe(shareReplay(1));
            } else {
                this.ruleCompletions = EMPTY;
            }
        }
    }
}
