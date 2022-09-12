/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AfterViewInit, ChangeDetectionStrategy, Component, ElementRef, Input, ViewChild } from '@angular/core';

@Component({
    selector: 'sqx-iframe-card',
    styleUrls: ['./iframe-card.component.scss'],
    templateUrl: './iframe-card.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class IFrameCardComponent implements AfterViewInit {
    @Input()
    public options: any;

    @ViewChild('iframe', { static: false })
    public iframe!: ElementRef<HTMLIFrameElement>;

    public ngAfterViewInit() {
        this.iframe.nativeElement.src = this.options?.src;
    }
}
