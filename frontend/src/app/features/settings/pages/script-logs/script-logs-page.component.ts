/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, inject } from '@angular/core';
import { FormAlertComponent, LayoutComponent, ListViewComponent, ScriptLogsState, ShortcutDirective, TitleComponent, TranslatePipe } from '@app/shared';
import { ScriptLogComponent } from './script-log.component';

@Component({
    selector: 'sqx-script-logs-page',
    styleUrls: ['./script-logs-page.component.scss'],
    templateUrl: './script-logs-page.component.html',
    imports: [
        FormAlertComponent,
        LayoutComponent,
        ListViewComponent,
        ScriptLogComponent,
        ShortcutDirective,
        TitleComponent,
        TranslatePipe,
    ],
})
export class ScriptLogsPageComponent {
    protected readonly scriptLogsState = inject(ScriptLogsState);

    constructor() {
        this.scriptLogsState.load();
    }

    protected reload() {
        this.scriptLogsState.load(true);
    }

    protected filter(name: string) {
        this.scriptLogsState.filter(name);
    }
}
