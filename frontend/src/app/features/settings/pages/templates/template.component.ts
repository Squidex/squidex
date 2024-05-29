/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { map, Observable, shareReplay } from 'rxjs';
import { AppsState, ClientsState, FormHintComponent, LoaderComponent, MarkdownPipe, SafeHtmlPipe, TemplateDetailsDto, TemplateDto, TemplatesService, TranslatePipe } from '@app/shared';

@Component({
    standalone: true,
    selector: 'sqx-template',
    styleUrls: ['./template.component.scss'],
    templateUrl: './template.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        AsyncPipe,
        FormHintComponent,
        LoaderComponent,
        MarkdownPipe,
        SafeHtmlPipe,
        TranslatePipe,
    ],
})
export class TemplateComponent {
    @Input({ required: true })
    public template!: TemplateDto;

    public isExpanded = false;

    public details?: Observable<string>;

    constructor(
        private readonly clientsState: ClientsState,
        private readonly appsState: AppsState,
        private readonly templatesService: TemplatesService,
    ) {
    }

    public ngOnChanges() {
        this.details = this.templatesService.getTemplate(this.template).pipe(map(x => this.buildDetails(x)), shareReplay(1));
    }

    public toggleExpanded() {
        this.isExpanded = !this.isExpanded;
    }

    private buildDetails(dto: TemplateDetailsDto) {
        const app = this.appsState.appName;

        let details = dto.details.replace(/<APP>/g, app);

        const client = this.clientsState.snapshot.clients[0];

        if (client) {
            const clientId = `${app}:${client.id}`;

            details = details.replace(/\<CLIENT_ID>/g, clientId);
            details = details.replace(/\<CLIENT_SECRET>/g, client.secret);
        }

        return details;
    }
}
