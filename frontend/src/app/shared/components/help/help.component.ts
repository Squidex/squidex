/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { LayoutComponent } from '@app/framework';
import { HelpService } from '@app/shared/internal';
import { HelpMarkdownPipe } from './help-markdown.pipe';

@Component({
    selector: 'sqx-help',
    styleUrls: ['./help.component.scss'],
    templateUrl: './help.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        AsyncPipe,
        HelpMarkdownPipe,
        LayoutComponent,
    ],
})
export class HelpComponent {
    private readonly helpService = inject(HelpService);
    private readonly route = inject(ActivatedRoute);

    protected helpMarkdown = this.helpService.getHelp(this.route.snapshot.data.helpPage);
}
