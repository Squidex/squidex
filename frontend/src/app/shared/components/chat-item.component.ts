/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { NgFor, NgIf } from '@angular/common';
import { ChangeDetectionStrategy, Component, ElementRef, EventEmitter, Input, Output, ViewChild } from '@angular/core';
import { Observable } from 'rxjs';
import { HTTP, MarkdownDirective, ResizedDirective, StatefulComponent, TranslatePipe, Types } from '@app/framework';
import { ChatEventDto, Profile } from '../internal';
import { UserIdPicturePipe } from './pipes';

interface State {
    // True, when running
    isRunning: boolean;

    // True, when failed
    isFailed: boolean;

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
        MarkdownDirective,
        NgFor,
        NgIf,
        ResizedDirective,
        TranslatePipe,
        UserIdPicturePipe,
    ],
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ChatItemComponent extends StatefulComponent<State> {
    @ViewChild('focusElement', { static: false })
    public focusElement!: ElementRef<HTMLElement>;

    @ViewChild('contentElement', { static: false })
    public contentElement!: ElementRef<HTMLElement>;

    @Input({ required: true })
    public type: 'Bot' | 'User' | 'System' = 'Bot';

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

    constructor() {
        super({
            content: '',
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

    public selectContent() {
        this.contentSelect.emit(this.snapshot.content);
    }

    public selectImage() {
        const image = this.contentElement.nativeElement?.querySelector('img');

        if (!image) {
            return;
        }

        const name = image.alt || 'image.webp';

        this.contentSelect.emit({ url: image.src, name });
    }
}