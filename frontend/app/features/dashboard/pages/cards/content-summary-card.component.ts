/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, ChangeDetectorRef, Component, Input, OnInit } from '@angular/core';
import { AppDto, ContentsService, fadeAnimation, Types } from '@app/shared';

@Component({
    selector: 'sqx-content-summary-card',
    styleUrls: ['./content-summary-card.component.scss'],
    templateUrl: './content-summary-card.component.html',
    animations: [
        fadeAnimation
    ],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class ContentSummaryCardComponent implements OnInit {
    @Input()
    public app: AppDto;

    @Input()
    public options: any;

    public itemCount = 0;

    constructor(
        private readonly changeDetector: ChangeDetectorRef,
        private readonly contentsService: ContentsService
    ) {
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
            .subscribe(dto => {
                this.itemCount = dto.total;

                this.changeDetector.detectChanges();
            },
            () => {
                this.itemCount = 0;
            });
    }
}