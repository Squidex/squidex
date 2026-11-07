/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, inject, input } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { catchError, map, of } from 'rxjs';
import { AppDto, ContentsService, TranslatePipe, Types } from '@app/shared';

@Component({
    selector: 'sqx-content-summary-card',
    styleUrls: ['./content-summary-card.component.scss'],
    templateUrl: './content-summary-card.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        TranslatePipe,
    ],
})
export class ContentSummaryCardComponent {
    private readonly contentsService = inject(ContentsService);

    /** The app of the dashboard. */
    public readonly app = input.required<AppDto>();

    /** The card configuration, with the name, the schema and the optional query to count the items. */
    public readonly options = input<any>();

    protected readonly itemCount = rxResource({
        params: () => {
            const options = this.options();
            if (!Types.isString(options?.schema)) {
                return undefined;
            }

            let query = options.query;

            if (!Types.isObject(query)) {
                query = {};
            }

            return { appName: this.app().name, schema: options.schema as string, query: { ...query, take: 0 } };
        },
        stream: ({ params: { appName, schema, query } }) => this.contentsService.getContents(appName, schema, { query }).pipe(
            map(({ total }) => total),
            catchError(() => of(0))),
        defaultValue: 0,
    });
}
