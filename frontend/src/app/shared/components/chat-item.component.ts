/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { ChangeDetectionStrategy, Component, ElementRef, EventEmitter, Input, Output, ViewChild } from '@angular/core';
import { lastValueFrom, Observable } from 'rxjs';
import { filter } from 'rxjs/operators';
import { ApiUrlConfig, HTTP, LoaderComponent, MarkdownDirective, markdownExtractImage, markdownHasImage, markdownTransformImages, ResizedDirective, StatefulComponent, TranslatePipe, Types } from '@app/framework';
import { AssetDto, AssetUploaderState, ChatEventDto, Profile } from '../internal';
import { UserIdPicturePipe } from './pipes';

interface State {
    // True, when running
    isRunning: boolean;

    // True, when failed
    isFailed: boolean;

    // True, when a copy is in process.
    isCopying: boolean;

    // The content.
    content: string;

    // The running tools.
    runningTools: string[];
}

@Component({
    standalone: true,
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
export class ChatItemComponent extends StatefulComponent<State> {
    @ViewChild('focusElement', { static: false })
    public focusElement!: ElementRef<HTMLElement>;

    @Input({ required: true })
    public type: 'Bot' | 'User' | 'System' = 'Bot';

    @Input({ required: true })
    public folderId?: string;

    @Input({ required: true })
    public user!: Profile;

    @Input({ required: true })
    public isLast: boolean = false;

    @Input({ required: true })
    public isFirst: boolean = false;

    @Input({ required: true })
    public copyMode?: 'Text' | 'Image';

    @Input({ required: true })
    public set content(value: string | Observable<ChatEventDto>) {
        if (Types.isString(value)) {
            this.next({ content: value });
        } else {
            this.next({ isRunning: true });

            value.subscribe({
                next: event => {
                    if (event.type === 'Chunk') {
                        this.next(s => ({
                            ...s,
                            content: s.content + event.content,
                        }));
                    } else if (event.type === 'ToolStart') {
                        this.next(s => ({
                            ...s,
                            runningTools: [...s.runningTools, event.tool],
                        }));
                    }
                },
                error: () => {
                    this.next({ isRunning: false, isFailed: true });
                    this.done.emit();
                },
                complete: () => {
                    this.next(s => ({
                        ...s,
                        isRunning: false,
                        isFailed: !s.content,
                    }));

                    this.done.emit();
                },
            });
        }
    }

    @Output()
    public done = new EventEmitter();

    @Output()
    public contentSelect = new EventEmitter<string | HTTP.UploadFile | undefined | null>();

    constructor(
        private readonly apiUrl: ApiUrlConfig,
        private readonly assetUploader: AssetUploaderState,
    ) {
        super({
            content: '',
            isCopying: false,
            isFailed: false,
            isRunning: false,
            runningTools: [],
        });

        this.changes.subscribe(() => {
            this.focusElement.nativeElement?.scrollIntoView();
        });
    }

    public scrollIntoView() {
        this.focusElement.nativeElement?.scrollIntoView();
    }

    public async selectContent() {
        let markdown = this.snapshot.content;

        if (!markdownHasImage(markdown)) {
            this.contentSelect.emit(markdown);
        }

        this.next({ isCopying: true });
        try {
            markdown = await markdownTransformImages(markdown, async img => {
                const asset = await lastValueFrom(
                    this.assetUploader.uploadFile(img, this.folderId)
                        .pipe(filter(x => Types.is(x, AssetDto)))) as AssetDto;

                return asset.fullUrl(this.apiUrl);
            });

            this.contentSelect.emit(markdown);
        } finally {
            this.next({ isCopying: false });
        }
    }

    public selectImage() {
        const image = markdownExtractImage(this.snapshot.content);

        if (!image) {
            return;
        }

        this.contentSelect.emit(image);
    }
}