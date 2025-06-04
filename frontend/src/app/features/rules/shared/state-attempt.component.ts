/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CodeEditorComponent, FlowExecutionStepAttemptDto, TranslatePipe, TypedSimpleChanges } from '@app/shared';

@Component({
    selector: 'sqx-state-attempt',
    styleUrls: ['./state-attempt.component.scss'],
    templateUrl: './state-attempt.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        CodeEditorComponent,
        FormsModule,
        TranslatePipe,
    ],
})
export class StateAttemptComponent {
    @Input({ required: true })
    public attempt!: FlowExecutionStepAttemptDto;

    public output?: string;

    public ngOnChanges(changes: TypedSimpleChanges<this>) {
        if (changes.attempt) {
            let log = '';

            for (const line of this.attempt.log) {
                log += `${line.timestamp.toISOString()}: ${line.message}\n`;

                if (line.dump) {
                    log += `${line.dump}\n\n`;
                }
            }

            this.output = log.replace(/[\r\n\s]+$/, '');
        }
    }
}