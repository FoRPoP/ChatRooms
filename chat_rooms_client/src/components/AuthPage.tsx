import React, { useState } from 'react';
import { PrimaryButton, TextField, Stack, Label, Link } from '@fluentui/react';
import { AuthApi } from '../api/apis/auth-api';
import { User } from '../api/models/user';

interface IAuthPage {
    onLogin: (username: string) => void;
}

const AuthPage: React.FC<IAuthPage> = ({ onLogin }) => {
    const [username, setUsername] = useState<string>('');
    const [password, setPassword] = useState<string>('');
    const [isLogin, setIsLogin] = useState<boolean>(true);
    const [message, setMessage] = useState<string | null>(null);
    
    const authApi = new AuthApi();

    const handleSubmit = async () => {
        if (username && password) {
            try {
                const user: User = { username: username, hashedPassword: password };
                if (isLogin) {
                    authApi.authLoginPost(user).then(response => {
                        if (response.data !== '') {
                            localStorage.setItem('jwtToken', response.data);
                            onLogin(username);
                        }
                        else {
                            setMessage('Authentication failed. Please try again.');
                        }
                    });
                } else {
                    authApi.authRegisterPost(user).then(response => {
                        response.data ? setMessage('User registered successfully!') : setMessage('User registration failed. Please try again.');
                    });
                }
                setUsername('');
                setPassword('');
            } catch (err) {
                setMessage('Authentication failed. Please try again.');
            }
        }
    };

    return (
        <Stack tokens={{ childrenGap: 15 }} styles={{ root: { width: 300, margin: '0 auto', padding: 20 } }} horizontalAlign='center'>
            <h2>{isLogin ? 'Login' : 'Register'}</h2>
            {message && <Label styles={{ root: { color: 'red' } }}>{message}</Label>}
            <TextField
                label="Username"
                value={username}
                onChange={(e, value) => setUsername(value!)}
                required
            />
            <TextField
                label="Password"
                type="password"
                value={password}
                onChange={(e, value) => setPassword(value!)}
                required
            />
            <PrimaryButton onClick={handleSubmit} text={isLogin ? 'Login' : 'Register'} />
            <Link onClick={() => setIsLogin(!isLogin)}>
                {isLogin ? 'Switch to Register' : 'Switch to Login'}
            </Link>
        </Stack>
    );
};

export default AuthPage;