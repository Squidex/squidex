/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, effect, input, signal, inject } from '@angular/core';
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
    /** The app of the dashboard. */
    public readonly app = input.required<AppDto>();

    /** The card configuration, with the name, the schema and the optional query to count the items. */
    public readonly options = input<any>();

    protected readonly itemCount = signal(0);

    constructor() {
        const contentsService = inject(ContentsService);

        effect(onCleanup => {
            const options = this.options();
            if (!Types.isString(options?.schema)) {
                return;
            }

            let query = options.query;

            if (!Types.isObject(query)) {
                query = {};
            }

            query = { ...query, take: 0 };

            const subscription =
                contentsService.getContents(this.app().name, options.schema, { query })
                    .subscribe({
                        next: ({ total: itemCount }) => {
                            this.itemCount.set(itemCount);
                        },
                        error: () => {
                            this.itemCount.set(0);
                        },
                    });

            onCleanup(() => subscription.unsubscribe());
        });
    }
}
