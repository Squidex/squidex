/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AfterViewInit, ChangeDetectionStrategy, Component, ElementRef, Input, ViewChild } from '@angular/core';
import { AppDto, fadeAnimation } from '@app/shared';

@Component({
    selector: 'sqx-iframe-card',
    styleUrls: ['./iframe-card.component.scss'],
    templateUrl: './iframe-card.component.html',
    animations: [
        fadeAnimation,
    ],
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class IFrameCardComponent implements AfterViewInit {
    @ViewChild('iframe', { static: false })
    public iframe: ElementRef<HTMLIFrameElement>;

    @Input()
    public app: AppDto;

    @Input()
    public options: any;

    public ngAfterViewInit() {
        this.iframe.nativeElement.src = this.options?.src;
    }
}
