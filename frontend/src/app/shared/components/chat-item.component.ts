/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { afterRenderEffect, booleanAttribute, ChangeDetectionStrategy, Component, effect, ElementRef, input, output, signal, viewChild } from '@angular/core';
import { lastValueFrom, Observable } from 'rxjs';
import { filter } from 'rxjs/operators';
import { ApiUrlConfig, HTTP, LoaderComponent, MarkdownDirective, markdownExtractImage, markdownHasImage, markdownTransformImages, ResizedDirective, TranslatePipe, Types } from '@app/framework';
import { AssetDto, AssetUploaderState, ChatEventDto, Profile } from '../internal';
import { UserIdPicturePipe } from './pipes';

@Component({
    selector: 'sqx-chat-item',
    styleUrls: ['./chat-item.component.scss'],
    templateUrl: './chat-item.component.html',
    imports: [
        LoaderComponent,
        MarkdownDirective,
        ResizedDirective,
        TranslatePipe,
        UserIdPicturePipe,
    ],
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ChatItemComponent {
    private readonly focusElement = viewChild<ElementRef<HTMLElement>>('focusElement');

    /** Emits when the answer has been completely received or has failed. */
    public readonly done = output();

    /** Emits the content (text or image) the user wants to use. */
    public readonly contentSelect = output<string | HTTP.UploadFile | undefined | null>();

    /** The author of the message. */
    public readonly type = input.required<'Assistant' | 'User' | 'System'>();

    /** The ID of the folder to upload images to, when the content is used. */
    public readonly folderId = input.required<string | undefined>();

    /** The current user, to show the profile picture of user messages. */
    public readonly user = input.required<Profile>();

    /** Indicates that this is the last message. */
    public readonly isLast = input(false, { transform: booleanAttribute });

    /** Indicates that this is the first message, which cannot be used. */
    public readonly isFirst = input(false, { transform: booleanAttribute });

    /** Defines what is copied when the content is used. */
    public readonly copyMode = input<'Text' | 'Image'>();

    /** The text of the message, or the stream of the answer. */
    public readonly content = input.required<string | Observable<ChatEventDto>>();

    public readonly contentText = signal('');
    public readonly contentTools = signal<ReadonlyArray<string>>([]);

    public readonly isRunning = signal(false);
    public readonly isFailed = signal(false);
    public readonly isCopying = signal(false);

    constructor(
        private readonly apiUrl: ApiUrlConfig,
        private readonly assetUploader: AssetUploaderState,
    ) {
        effect(onCleanup => {
            const content = this.content();
            if (Types.isString(content)) {
                this.contentText.set(content);
                return;
            }

            this.isRunning.set(true);

            const subscription = content.subscribe({
                next: event => {
                    if (event.type === 'Chunk') {
                        this.contentText.update(s => s + event.content);
                    } else if (event.type === 'ToolStart') {
                        this.contentTools.update(s => [...s, event.tool]);
                    }
                },
                error: () => {
                    this.isRunning.set(false);
                    this.isFailed.set(true);
                    this.done.emit();
                },
                complete: () => {
                    this.isRunning.set(false);
                    this.isFailed.set(!this.contentText());
                    this.done.emit();
                },
            });

            onCleanup(() => subscription.unsubscribe());
        });

        afterRenderEffect(() => {
            this.contentText();
            this.contentTools();
            this.isRunning();
            this.isFailed();
            this.isCopying();

            this.scrollIntoView();
        });
    }

    public scrollIntoView() {
        this.focusElement()?.nativeElement.scrollIntoView();
    }

    public async selectContent() {
        let markdown = this.contentText();

        if (!markdownHasImage(markdown)) {
            this.contentSelect.emit(markdown);
            return;
        }

        this.isCopying.set(true);
        try {
            markdown = await markdownTransformImages(markdown, async img => {
                const asset = await lastValueFrom(
                    this.assetUploader.uploadFile(img, this.folderId())
                        .pipe(filter(x => Types.is(x, AssetDto)))) as AssetDto;

                return asset.fullUrl(this.apiUrl);
            });

            this.contentSelect.emit(markdown);
        } finally {
            this.isCopying.set(false);
        }
    }

    public selectImage() {
        const image = markdownExtractImage(this.contentText());
        if (!image) {
            return;
        }

        this.contentSelect.emit(image);
    }
}
