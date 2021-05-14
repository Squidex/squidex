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

    public imageSource: string | null;
    public imageSize = '50px';

    public ngOnChanges(changes: SimpleChanges) {
        if (changes['image'] || changes['identifier']) {
            this.imageSource = this.image || this.createSvg();
        }

        if (changes['size']) {
            this.imageSize = `${this.size}px`;
        }
    }

    private createSvg() {
        if (this.identifier) {
            return `data:image/svg+xml;utf8,${picasso(this.identifier)}`;
        } else {
            return null;
        }
    }
}
