/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { FromNowPipe, ScriptLogDto, TranslatePipe, TypedSimpleChanges } from '@app/shared';

@Component({
    selector: 'sqx-script-log',
    styleUrls: ['./script-log.component.scss'],
    templateUrl: './script-log.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        FromNowPipe,
        TranslatePipe,
    ],
})
export class ScriptLogComponent {
    @Input({ required: true })
    public scriptLog!: ScriptLogDto;

    public lines: ReadonlyArray<{ time: string; level: string; message: string }> = [];

    public omitted = 0;

    public isExpanded = false;

    public ngOnChanges(changes: TypedSimpleChanges<this>) {
        if (changes.scriptLog) {
            this.lines = this.scriptLog.entries.map(x => ({
                time: x.timestamp?.toStringFormat('yyyy-MM-dd HH:mm:ss.SSS') ?? '',
                level: x.level,
                message: x.message,
            }));

            this.omitted = Math.max(0, this.scriptLog.totalEntries - this.scriptLog.entries.length);
        }
    }

    public toggleExpanded() {
        this.isExpanded = !this.isExpanded;
    }
}
