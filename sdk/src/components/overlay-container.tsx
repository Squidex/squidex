import { h } from 'preact';
import { useCallback, useEffect, useRef, useState } from 'preact/hooks';
import { IFrame } from './iframe';
import { Overlay } from './Overlay';
import { TokenInfo } from './shared';

export interface OverlayContainerProps {
    // The base url of the script.
    baseUrl: string | null | undefined;
}

type AuthState = 'Authenticated' | 'Failed' | 'Pending';

const UNSET = { x: Number.NEGATIVE_INFINITY, y: Number.NEGATIVE_INFINITY };

export const OverlayContainer = (props: OverlayContainerProps) => {
    const div = useRef<any>();
    const [auth, setAuth] = useState<{ [url: string]: AuthState }>({});
    const [target, setTarget] = useState<{ target: HTMLElement, token: TokenInfo }>();
    const [targetUrl, setTargetUrl] = useState<string>();
    const authRef = useRef(auth);
    
    useEffect(() => {
        let previousElement: any;
        let previousTarget: HTMLElement | undefined = undefined;
        let previousPosition = UNSET;

        const updateTarget = (target?: HTMLElement, token?: TokenInfo) => {
            if (target !== previousTarget) {
                if (target && token) {
                    setTarget({ target, token });
                } else {
                    setTarget(undefined);
                }

                previousTarget = target;
            }
        };

        function listen(event: MouseEvent) {
            const element = event.target as HTMLElement;

            if (!element || isToolbar(element)) {
                return;
            }

            const { token, target } = parseTokenInPath(element, Object.keys(authRef.current));
            const position = { x: event.clientX, y: event.clientY };

            if (token && target) {                        
                updateTarget(target, token);

                previousPosition = position;
            } else if (Math.abs(position.x - previousPosition.x) + Math.abs(position.y - previousPosition.y) > 20) {
                updateTarget(undefined, undefined);
            }

            previousElement = element;
        }

        document.addEventListener('mousemove', listen);

        return () => {
            document.removeEventListener('mousemove', listen);
        }
    }, []);

    const checkAuth = useCallback((url: string | null | undefined) => {
        if (!url) {
            return;
        }

        if (authRef.current[url]) {
            return;
        }

        const updateAuth = (state: AuthState) => {
            const newAuth = {
                ...authRef.current,
                [url]: state
            };

            authRef.current = newAuth;

            setAuth(newAuth);
        };

        if (url.indexOf('http://') === 0) {
            updateAuth('Authenticated');
            return;
        }

        const fetchStatus = async () => {
            updateAuth('Pending');

            try {
                const response = await fetch(`${url}/identity-server/info`, {
                    credentials: 'include'
                });

                const json = await response.json();

                updateAuth(json.displayName ? 'Authenticated' : 'Failed');
            } catch {
                updateAuth('Failed');
            }
        };

        fetchStatus();
    }, [auth]);

    useEffect(() => {
        checkAuth(props.baseUrl);
    }, [props.baseUrl]);

    useEffect(() => {
        checkAuth(target?.token.u);
    }, [target?.token.u]);
    
    const isAuthenticated = auth[target?.token.u!] === 'Authenticated'

    return (
        <div class='squidex' ref={div}>
            {target && isAuthenticated &&
                <Overlay onOpen={setTargetUrl} {...target} />
            }

            {targetUrl &&
                <IFrame url={targetUrl} onClose={() => setTargetUrl(undefined)} />
            }
        </div>
    );
}

const CDN_URL = 'https://assets.squidex.io';

function isToolbar(target: HTMLElement) {
    let current = target;

    while (current) {
        if (current.className === 'squidex-overlay-toolbar') {
            return true;
        }

        current = current.parentElement as HTMLElement;
    }

    return false;
}

function parseTokenInPath(target: HTMLElement, baseUrls: string[]): { token?: TokenInfo, target?: HTMLElement } {
    let current = target;

    while (current) {
        const token = parseToken(current, baseUrls);

        if (token) {
            return { token, target: current };
        }

        current = current.parentElement as HTMLElement;
    }

    return {};
}

function parseToken(target: HTMLElement, baseUrls: string[]): TokenInfo | null {
    const value = target.getAttribute('squidex-token');

    if (!value && target.nodeName === 'IMG') {
        const src = (target as any)['src'] as string;

        if (src) {
            for (const baseUrl of baseUrls) {
                if (src.indexOf(baseUrl) === 0) {
                    const parts = src.substring(baseUrl.length + 1).split('/');

                    if (parts[0] === 'api' &&
                        parts[1] === 'assets' &&
                        parts[2]?.length > 0 &&
                        parts[3]?.length > 0) {
                        return {
                            u: baseUrl,
                            a: parts[2],
                            i: parts[3]
                        };
                    }
                }
            }

            if (src.indexOf(CDN_URL) === 0) {
                const parts = src.substring(CDN_URL.length + 1).split('/');

                if (parts[0]?.length > 0 &&
                    parts[1]?.length > 0) {
                    return {
                        u: CDN_URL,
                        a: parts[0],
                        i: parts[1]
                    };
                }

            }
        }
    }

    if (!value) {
        return null;
    }
    
    try {
        const decoded = atob(value);

        let token = JSON.parse(decoded) as TokenInfo;

        if (!token.u || !token.i || !token.a) {
            return null;
        }

        while (token.u.endsWith('/')) {
            token.u = token.u.substring(0, token.u.length - 1);
        }

        return token;
    } catch {
        return null;
    }
}