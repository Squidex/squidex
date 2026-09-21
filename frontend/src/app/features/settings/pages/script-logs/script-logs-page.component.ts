/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { FormAlertComponent, LayoutComponent, ListViewComponent, ScriptLogsState, ShortcutDirective, TitleComponent, TranslatePipe } from '@app/shared';
import { ScriptLogComponent } from './script-log.component';

@Component({
    selector: 'sqx-script-logs-page',
    styleUrls: ['./script-logs-page.component.scss'],
    templateUrl: './script-logs-page.component.html',
    imports: [
        AsyncPipe,
        FormAlertComponent,
        FormsModule,
        LayoutComponent,
        ListViewComponent,
        ScriptLogComponent,
        ShortcutDirective,
        TitleComponent,
        TranslatePipe,
    ],
})
export class ScriptLogsPageComponent implements OnInit {
    constructor(
        public readonly scriptLogsState: ScriptLogsState,
    ) {
    }

    public ngOnInit() {
        this.scriptLogsState.load();
    }

    public reload() {
        this.scriptLogsState.load(true);
    }

    public filter(name: string) {
        this.scriptLogsState.filter(name);
    }
}
