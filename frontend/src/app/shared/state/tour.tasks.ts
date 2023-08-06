/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { inject, InjectionToken } from '@angular/core';
import { filter, Observable, take } from 'rxjs';
import { StepDefinition, waitForAnchor } from '@app/framework';
import { AppsState } from './apps.state';
import { ContentsState } from './contents.state';
import { SchemasState } from './schemas.state';

export const TASK_CONFIGURATION = new InjectionToken<TaskConfiguration>('Tasks');

export interface TaskConfiguration {
    defaults: TaskDefinition;

    // The actual tasks.
    tasks: TaskDefinition[];
}

export interface TaskDefinition {
    // The ID to store it in the backend.
    id: string;

    // The title.
    title: string;

    // The description.
    description: string;

    // The steps to operate.
    steps: StepDefinition[];

    // Indicates that the task has been completed.
    onComplete: Observable<any>;
}

export function buildTasks() {
   const tasks: TaskDefinition[] = [{
        id: 'createApp',
        title: 'i18n:tour.createApp.title',
        description: 'i18n:tour.createApp.description',
        steps: [{
            anchorId: 'addApp',
            content: 'i18n:tour.createApp.buttonContent',
            title: 'i18n:tour.createApp.buttonTitle',
            nextOnAnchorClick: true,
            scrollContainer: '.panel-container',
            position: 'top-left',
        }, {
            anchorId: 'appForm',
            content: 'i18n:tour.createApp.formContent',
            title: 'i18n:tour.createApp.formTitle',
            nextOnCondition: waitForAnchor('app'),
            scrollContainer: '.panel-container',
            position: 'left-top',
        }, {
            anchorId: 'app',
            content: 'i18n:tour.createApp.selectContent',
            title: 'i18n:tour.createApp.selectTitle',
            nextOnAnchorClick: true,
            scrollContainer: '.panel-container',
        }, {
            anchorId: 'appDashboard',
            content: 'i18n:tour.createApp.dashboardContent',
            title: 'i18n:tour.createApp.dashboardTitle',
            isAsync: true,
            position: 'right-top',
        }],
        onComplete: (() => {
            const appsState = inject(AppsState);

            return appsState.changes.pipe(
                filter(change =>
                    change.snapshot.apps.length > 0),
                take(1));
        })(),
    }, {
        id: 'createSchema',
        title: 'i18n:tour.createSchema.title',
        description: 'i18n:tour.createSchema.description',
        steps: [/*{
            anchorId: 'app',
            content: 'i18n:tour.createSchema.selectAppContent',
            title: 'i18n:tour.createApp.selectAppTitle',
            isOptional: true,
            nextOnAnchorClick: true,
            scrollContainer: '.panel-container',
        }, {
            anchorId: 'schemas',
            content: 'i18n:tour.createSchema.selectSchemasContent',
            title: 'i18n:tour.createSchema.selectSchemasTitle',
            nextOnAnchorClick: true,
            scrollContainer: '.panel-container',
        }, {
            anchorId: 'addSchema',
            content: 'i18n:tour.createSchema.buttonContent',
            title: 'i18n:tour.createSchema.buttonTitle',
            isAsync: true,
            nextOnAnchorClick: true,
            scrollContainer: '.panel-container',
        }, {
            anchorId: 'schemaForm',
            content: 'i18n:tour.createSchema.formContent',
            title: 'i18n:tour.createSchema.formTitle',
            nextOnCondition: waitForAnchor('schema'),
            scrollContainer: '.panel-container',
        }, {
            anchorId: 'schema',
            content: 'i18n:tour.createSchema.selectSchemaContent',
            title: 'i18n:tour.createSchema.selectSchemaTitle',
            nextOnAnchorClick: true,
            scrollContainer: '.panel-container',
        }, {
            anchorId: 'addField',
            content: 'i18n:tour.createSchema.addFieldButtonContent',
            title: 'i18n:tour.createSchema.addFieldButtonTitle',
            nextOnAnchorClick: true,
            scrollContainer: '.panel-container',
        }, {
            anchorId: 'fieldForm',
            content: 'i18n:tour.createSchema.fieldFormContent',
            title: 'i18n:tour.createSchema.fieldFormTitle',
            nextOnCondition: waitForAnchor('schemafield'),
            scrollContainer: '.panel-container',
            position: 'left-top',
        }, {
            anchorId: 'schemaField',
            content: 'i18n:tour.createSchema.schemaFieldContent',
            title: 'i18n:tour.createSchema.schemaFieldTitle',
            scrollContainer: '.panel-container',
        }, */{
            anchorId: 'publishSchema',
            content: 'i18n:tour.createSchema.publishContent',
            title: 'i18n:tour.createSchema.publishTitle',
            nextOnAnchorClick: true,
            scrollContainer: '.panel-container',
        }, {
            anchorId: 'help',
            content: 'i18n:tour.createSchema.helpContent',
            title: 'i18n:tour.createSchema.helpTitle',
            scrollContainer: '.panel-container',
            position: 'left-center',
        }, {
            anchorId: 'history',
            content: 'i18n:tour.createSchema.historyContent',
            title: 'i18n:tour.createSchema.historyTitle',
            scrollContainer: '.panel-container',
            position: 'left-center',
        }],
        onComplete: (() => {
            const schemasState = inject(SchemasState);

            return schemasState.changes.pipe(
                filter(change =>
                    change.snapshot.schemas.find(s => s.fields.length > 0 && s.isPublished) !== undefined),
                take(1));
        })(),
    }, {
        id: 'createContent',
        title: 'tour.createContent.title',
        description: 'tour.createContent.description',
        steps: [{
            anchorId: 'selectSchema',
            content: 'Use this button to create a new app.',
            title: 'Add a new app',
            nextOnAnchorClick: true,
            scrollContainer: '.panel-container',
        }, {
            anchorId: 'appForm',
            content: 'Other content',
            title: 'Second',
        }],
        onComplete: (() => {
            const contentsState = inject(ContentsState);

            return contentsState.changes.pipe(
                filter(change =>
                    change.snapshot.contents.find(s => s.status === 'Published') !== undefined ||
                    change.snapshot.selectedContent?.status === 'Published'),
                take(1));
        })(),
    }, {
        id: 'createContent',
        title: 'tour.createContent.title',
        description: 'tour.createContent.description',
        steps: [{
            anchorId: 'selectSchema',
            content: 'Use this button to create a new app.',
            title: 'Add a new app',
            nextOnAnchorClick: true,
            scrollContainer: '.panel-container',
        }, {
            anchorId: 'appForm',
            content: 'Other content',
            title: 'Second',
        }],
        onComplete: (() => {
            const contentsState = inject(ContentsState);

            return contentsState.changes.pipe(
                filter(change =>
                    change.snapshot.contents.find(s => s.status === 'Published') !== undefined ||
                    change.snapshot.selectedContent?.status === 'Published'),
                take(1));
        })(),
    }, {
        id: 'createContent',
        title: 'tour.createContent.title',
        description: 'tour.createContent.description',
        steps: [{
            anchorId: 'selectSchema',
            content: 'Use this button to create a new app.',
            title: 'Add a new app',
            nextOnAnchorClick: true,
            scrollContainer: '.panel-container',
        }, {
            anchorId: 'appForm',
            content: 'Other content',
            title: 'Second',
        }],
        onComplete: (() => {
            const contentsState = inject(ContentsState);

            return contentsState.changes.pipe(
                filter(change =>
                    change.snapshot.contents.find(s => s.status === 'Published') !== undefined ||
                    change.snapshot.selectedContent?.status === 'Published'),
                take(1));
        })(),
    }];

    const defaults: StepDefinition = {
        allowUserInitiatedNavigation: true,
        enableBackdrop: true,
        duplicateAnchorHandling: 'registerLast',
    };

    return { tasks, defaults };
}