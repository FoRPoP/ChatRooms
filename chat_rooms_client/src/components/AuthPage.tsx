import React, { useState } from 'react';
import { PrimaryButton, TextField, Stack } from '@fluentui/react';

interface IAuthPage {
    onLogin: (username: string) => void;
}

const AuthPage: React.FC<IAuthPage> = ({ onLogin }) => {
    const [username, setUsername] = useState<string>('');

    const handleLogin = () => {
        if (username) {
            onLogin(username);
        }
    };

    return (
        <Stack tokens={{ childrenGap: 15 }} styles={{ root: { width: 300, margin: '0 auto', padding: 20 } }} horizontalAlign='center'>
            <h2>Login</h2>
            <TextField
                label="Username"
                value={username}
                onChange={(e, value) => setUsername(value!)}
                required
            />
            <PrimaryButton onClick={handleLogin} text='Login' />
        </Stack>
    );
};

export default AuthPage;