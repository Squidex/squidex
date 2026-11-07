/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
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
        HelpMarkdownPipe,
        LayoutComponent,
    ],
})
export class HelpComponent {
    private readonly helpService = inject(HelpService);
    private readonly route = inject(ActivatedRoute);

    protected readonly helpMarkdown = rxResource({
        stream: () => this.helpService.getHelp(this.route.snapshot.data.helpPage),
    });
}
