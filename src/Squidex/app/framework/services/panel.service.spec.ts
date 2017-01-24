/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { PanelService, PanelServiceFactory } from './../';

interface Styling { element: any; property: string; value: string; }

describe('PanelService', () => {
    it('should instantiate from factory', () => {
        const panelService = PanelServiceFactory();

        expect(panelService).toBeDefined();
    });

    it('should instantiate', () => {
        const panelService = new PanelService();

        expect(panelService).toBeDefined();
    });

    it('should update elements with renderer service', () => {
        let styles: Styling[] = [];

        const renderer = {
            setElementStyle: (element: any, property: string, value: string) => {
                styles.push({element, property, value});
            }
        };

        const panelService = new PanelService();

        const element1 = {
            getBoundingClientRect: () => {
                return { width: 100 };
            }
        };

        const element2 = {
            getBoundingClientRect: () => {
                return { width: 200 };
            }
        };

        const element3 = {
            getBoundingClientRect: () => {
                return { width: 300 };
            }
        };

        let numPublished = 0;
        panelService.changed.subscribe(() => {
            numPublished++;
        });

        panelService.push(element1, <any>renderer);
        panelService.push(element2, <any>renderer);
        panelService.push(element3, <any>renderer);

        styles = [];

        panelService.pop(element3, <any>renderer);

        expect(styles).toEqual([
            { element: element1, property: 'top', value: '0px' },
            { element: element1, property: 'left', value: '0px' },
            { element: element1, property: 'bottom', value: '0px' },
            { element: element1, property: 'position', value: 'absolute' },
            { element: element1, property: 'z-index', value: '20' },

            { element: element2, property: 'top', value: '0px' },
            { element: element2, property: 'left', value: '100px' },
            { element: element2, property: 'bottom', value: '0px' },
            { element: element2, property: 'position', value: 'absolute' },
            { element: element2, property: 'z-index', value: '10' }
        ]);

        expect(numPublished).toBe(4);
    });
});