/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, computed, input, linkedSignal, numberAttribute } from '@angular/core';
import { picasso } from '@app/framework/internal';
import { SafeUrlPipe } from './safe-html.pipe';

@Component({
    selector: 'sqx-avatar',
    styleUrls: ['./avatar.component.scss'],
    templateUrl: './avatar.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        SafeUrlPipe,
    ],
})
export class AvatarComponent {
    private readonly actualImage = linkedSignal(() => this.image());

    /** The identifier to generate the fallback image from, for example the user ID. */
    public readonly identifier = input<string | undefined | null>();

    /** The URL of the image. Falls back to a generated image when not set or not loadable. */
    public readonly image = input<string | undefined | null>();

    /** The width and height of the avatar in pixels. */
    public readonly size = input(50, { transform: numberAttribute });

    protected readonly imageSource = computed(() => {
        return this.actualImage() || this.createSvg(this.identifier());
    });

    protected readonly imageSize = computed(() => {
        return `${this.size()}px`;
    });

    protected unsetImage() {
        this.actualImage.set(null);
    }

    private createSvg(identifier: string | undefined | null) {
        if (identifier) {
            return `data:image/svg+xml;utf8,${picasso(identifier)}`;
        } else {
            return null;
        }
    }
}