/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, ChangeDetectorRef, Component, Input, OnInit } from '@angular/core';
import { AppDto, ContentsService, StatefulComponent, Types } from '@app/shared';

interface State {
    // The number of items.
    itemCount: number;
}

@Component({
    selector: 'sqx-content-summary-card[app]',
    styleUrls: ['./content-summary-card.component.scss'],
    templateUrl: './content-summary-card.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ContentSummaryCardComponent extends StatefulComponent<State> implements OnInit {
    @Input()
    public app!: AppDto;

    @Input()
    public options?: any;

    constructor(changeDetector: ChangeDetectorRef,
        private readonly contentsService: ContentsService,
    ) {
        super(changeDetector, {
            itemCount: 0,
        });
    }

    public ngOnInit() {
        if (!Types.isString(this.options?.schema)) {
            return;
        }

        let query = this.options?.query;

        if (!Types.isObject(query)) {
            query = {};
        }

        query.take = 0;

        this.contentsService.getContents(this.app.name, this.options.schema, { query })
            .subscribe({
                next: ({ total: itemCount }) => {
                    this.next({ itemCount });
                },
                error: () => {
                    this.next({ itemCount: 0 });
                },
            });
    }
}
