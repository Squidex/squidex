/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { ChangeDetectionStrategy, Component, Input, numberAttribute } from '@angular/core';
import { picasso, TypedSimpleChanges } from '@app/framework/internal';
import { SafeUrlPipe } from './safe-html.pipe';

@Component({
    standalone: true,
    selector: 'sqx-avatar',
    styleUrls: ['./avatar.component.scss'],
    templateUrl: './avatar.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        SafeUrlPipe,
    ],
})
export class AvatarComponent {
    @Input()
    public identifier: string | undefined | null;

    @Input()
    public image: string | undefined | null;

    @Input({ transform: numberAttribute })
    public size = 50;

    public imageSource?: string | null;
    public imageSize = '50px';

    public actualImage: string | undefined | null;

    public ngOnChanges(changes: TypedSimpleChanges<this>) {
        if (changes.image) {
            this.actualImage = this.image;
        }

        if (changes.image || changes.identifier) {
            this.updateSource();
        }

        if (changes.size) {
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
