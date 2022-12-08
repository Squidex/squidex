/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, Input, OnChanges, SimpleChanges } from '@angular/core';
import { picasso } from '@app/framework/internal';

@Component({
    selector: 'sqx-avatar',
    styleUrls: ['./avatar.component.scss'],
    templateUrl: './avatar.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AvatarComponent implements OnChanges {
    @Input()
    public identifier: string | undefined | null;

    @Input()
    public image: string | undefined | null;

    @Input()
    public size = 50;

    public imageSource?: string | null;
    public imageSize = '50px';

    public actualImage: string | undefined | null;

    public ngOnChanges(changes: SimpleChanges) {
        if (changes['image']) {
            this.actualImage = this.image;
        }

        if (changes['image'] || changes['identifier']) {
            this.updateSource();
        }

        if (changes['size']) {
            this.imageSize = `${this.size}px`;
        }
    }

    private updateSource() {
        this.imageSource = this.actualImage || this.createSvg();
    }

    public unsetImage() {
        this.actualImage = null;

        this.updateSource();
    }

    private createSvg() {
        if (this.identifier) {
            return `data:image/svg+xml;utf8,${picasso(this.identifier)}`;
        } else {
            return null;
        }
    }
}
